package rabbitmq

import (
	"fmt"
	"sync"

	amqp "github.com/rabbitmq/amqp091-go"
)

type Connection struct {
	conn    *amqp.Connection
	channel *amqp.Channel
	mu      sync.RWMutex
	queue   string
}

func NewConnection(host, port, user, password, queue string) (*Connection, error) {
	url := fmt.Sprintf("amqp://%s:%s@%s:%s/", user, password, host, port)
	conn, err := amqp.Dial(url)
	if err != nil {
		return nil, err
	}

	ch, err := conn.Channel()
	if err != nil {
		return nil, err
	}

	q, err := ch.QueueDeclare(
		queue,
		true,  // durable
		false, // autoDelete
		false, // exclusive
		false, // noWait
		nil,   // args
	)

	if err == nil {
		fmt.Printf("Queue created with name: %s\n", q.Name)
		fmt.Printf("Messages count: %d\n", q.Messages)
	}

	return &Connection{
		conn:    conn,
		channel: ch,
		queue:   queue,
	}, nil
}

func (c *Connection) GetChannel() *amqp.Channel {
	c.mu.RLock()
	defer c.mu.RUnlock()
	return c.channel
}

func (c *Connection) Close() error {
	c.mu.Lock()
	defer c.mu.Unlock()

	if err := c.channel.Close(); err != nil {
		return err
	}

	return c.conn.Close()
}
