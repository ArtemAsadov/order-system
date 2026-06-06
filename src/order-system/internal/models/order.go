package models

import (
	"fmt"
	"time"
)

type Order struct {
	OrderID    int64     `json:"order_id"`
	UserID     int64     `json:"user_id"`
	Category   string    `json:"category"`
	Amount     int       `json:"amount"`
	UnitPrice  float64   `json:"unit_price"`
	TotalPrice float64   `json:"total_price"`
	CreatedAt  time.Time `json:"created_at"`
}

func (o *Order) CalculateTotal() {
	o.TotalPrice = float64(o.Amount) * o.UnitPrice
}

type OrderPlacedCommand struct {
	MessageID   string    `json:"message_id"`
	MessageType string    `json:"message_type"`
	Order       Order     `json:"order"`
	CreatedAt   time.Time `json:"created_at"`
}

func NewOrderPlacedCommand(order Order) *OrderPlacedCommand {
	return &OrderPlacedCommand{
		MessageID:   generateID(),
		MessageType: "OrderPlacedCommand",
		Order:       order,
		CreatedAt:   time.Now(),
	}
}

func generateID() string {
	return fmt.Sprintf("%d", time.Now().UnixNano())
}
