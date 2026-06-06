package rabbitmq

import (
	"context"
	"encoding/json"

	amqp "github.com/rabbitmq/amqp091-go"
)

type Publisher struct {
	conn *Connection
}

func NewPublisher(conn *Connection) *Publisher {
	return &Publisher{conn: conn}
}

func (p *Publisher) Publish(ctx context.Context, message interface{}) error {
	body, err := json.Marshal(message)
	if err != nil {
		return err
	}

	return p.conn.GetChannel().PublishWithContext(
		ctx,
		"",           // exchange
		p.conn.queue, // routing key
		false,        // mandatory
		false,        // immediate
		amqp.Publishing{
			ContentType: "application/json",
			Body:        body,
		},
	)
}
