package generator

import (
	"context"
	"fmt"
	"math/rand"
	"order-system/internal/models"
	"order-system/internal/rabbitmq"
	"sync/atomic"
	"time"
)

type Config struct {
	RPS        int
	MaxUsers   int
	Categories []string
}

type Service struct {
	config     Config
	publisher  *rabbitmq.Publisher
	isRunning  atomic.Bool
	cancelFunc context.CancelFunc
	totalSent  atomic.Int64
}

func NewService(cfg Config, publisher *rabbitmq.Publisher) *Service {
	return &Service{
		config:    cfg,
		publisher: publisher,
	}
}

func (s *Service) Start() error {
	if s.isRunning.Swap(true) {
		return nil
	}

	ctx, cancel := context.WithCancel(context.Background())
	s.cancelFunc = cancel

	go s.run(ctx)
	return nil
}

func (s *Service) Stop() {
	if !s.isRunning.Swap(false) {
		return
	}

	if s.cancelFunc != nil {
		s.cancelFunc()
	}
}

func (s *Service) IsRunning() bool {
	return s.isRunning.Load()
}
func (s *Service) TotalSent() int64 {
	return s.totalSent.Load()
}

func (s *Service) run(ctx context.Context) {
	delayMs := 1000 / s.config.RPS

	if delayMs < 1 {
		delayMs = 1
	}

	var orderID int64 = 0

	ticker := time.NewTicker(time.Duration(delayMs) * time.Millisecond)
	defer ticker.Stop()

	for {
		select {
		case <-ctx.Done():
			return
		case <-ticker.C:
			orderID++
			order := s.generateOrder(orderID)
			command := models.NewOrderPlacedCommand(order)

			if err := s.publisher.Publish(ctx, command); err != nil {
				fmt.Printf("failed push %d, %v \n", orderID, err)
				continue
			}

			s.totalSent.Add(1)

			if orderID%1000 == 0 {
				fmt.Printf("📊 Sent %d orders (RPS: %d)\n", orderID, s.config.RPS)
			}
		}
	}
}

func (s *Service) generateOrder(id int64) models.Order {
	r := rand.New(rand.NewSource(time.Now().UnixNano()))

	unitPrice := float64(r.Intn(10000)+100) / 100
	amount := r.Intn(10) + 1
	category := s.config.Categories[r.Intn(len(s.config.Categories))]
	userID := r.Int63n(int64(s.config.MaxUsers)) + 1

	order := models.Order{
		OrderID:   id,
		UserID:    userID,
		Category:  category,
		Amount:    amount,
		UnitPrice: unitPrice,
		CreatedAt: time.Now(),
	}
	order.CalculateTotal()

	return order
}
