package main

import (
	"context"
	"encoding/json"
	"log"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"

	"order-system/internal/generator"
	"order-system/internal/rabbitmq"

	"github.com/gorilla/mux"
)

type App struct {
	server    *http.Server
	generator *generator.Service
	stopCh    chan struct{}
}

func main() {
	app := &App{
		stopCh: make(chan struct{}),
	}
	app.run()
}

func (a *App) run() {
	// ========== Конфиги ==========
	rabbitConfig := struct {
		Host     string
		Port     string
		User     string
		Password string
		Queue    string
	}{
		Host:     "localhost",
		Port:     "5672",
		User:     "orderuser",
		Password: "orderpass",
		Queue:    "orders",
	}

	genConfig := generator.Config{
		RPS:      6000,
		MaxUsers: 1000,
		Categories: []string{
			"electronics", "clothing", "books", "food", "sports",
		},
	}

	// ========== RabbitMQ ==========
	conn, err := rabbitmq.NewConnection(
		rabbitConfig.Host,
		rabbitConfig.Port,
		rabbitConfig.User,
		rabbitConfig.Password,
		rabbitConfig.Queue,
	)
	if err != nil {
		log.Fatalf("Failed to connect to RabbitMQ: %v", err)
	}
	defer conn.Close()

	publisher := rabbitmq.NewPublisher(conn)
	a.generator = generator.NewService(genConfig, publisher)

	// ========== HTTP Сервер ==========
	r := mux.NewRouter()

	// API ручки
	r.HandleFunc("/api/generator/start", a.handleStart()).Methods("POST")
	r.HandleFunc("/api/generator/stop", a.handleStop()).Methods("POST")
	r.HandleFunc("/api/generator/cancel", a.handleCancel()).Methods("POST")
	r.HandleFunc("/api/generator/status", a.handleStatus()).Methods("GET")

	a.server = &http.Server{
		Addr:         ":8080",
		Handler:      r,
		ReadTimeout:  5 * time.Second,
		WriteTimeout: 10 * time.Second,
		IdleTimeout:  15 * time.Second,
	}

	// ========== Запуск ==========
	go a.startServer()
	a.waitForShutdown()
}

func (a *App) startServer() {
	log.Println("🚀 Go Generator starting on :8080")
	log.Println("   POST   /api/generator/start  - start generation")
	log.Println("   POST   /api/generator/stop   - stop generation")
	log.Println("   POST   /api/generator/cancel - shutdown server")
	log.Println("   GET    /api/generator/status - get status")

	if err := a.server.ListenAndServe(); err != nil && err != http.ErrServerClosed {
		log.Fatalf("Server failed: %v", err)
	}
}

func (a *App) waitForShutdown() {
	sigCh := make(chan os.Signal, 1)
	signal.Notify(sigCh, syscall.SIGINT, syscall.SIGTERM)

	select {
	case <-sigCh:
		log.Println("\n🛑 Received SIGTERM, shutting down...")
	case <-a.stopCh:
		log.Println("\n🛑 Received API cancel, shutting down...")
	}

	// Останавливаем генератор
	a.generator.Stop()

	// Graceful shutdown сервера
	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()

	if err := a.server.Shutdown(ctx); err != nil {
		log.Printf("Server shutdown error: %v", err)
	}

	log.Println("✅ Server exited gracefully")
}

// ========== Handlers ==========

func (a *App) handleStart() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if err := a.generator.Start(); err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}
		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(map[string]interface{}{
			"running": true,
			"message": "Generator started",
		})
	}
}

func (a *App) handleStop() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		a.generator.Stop()
		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(map[string]interface{}{
			"running": false,
			"message": "Generator stopped (server still running)",
		})
	}
}

func (a *App) handleCancel() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(map[string]interface{}{
			"status":  "shutting_down",
			"message": "Server will shut down in 1 second",
		})

		// Даём время отправить ответ
		go func() {
			time.Sleep(1 * time.Second)
			close(a.stopCh)
		}()
	}
}

func (a *App) handleStatus() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(map[string]interface{}{
			"running":    a.generator.IsRunning(),
			"total_sent": a.generator.TotalSent(),
		})
	}
}
