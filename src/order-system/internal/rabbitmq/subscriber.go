package rabbitmq

import "context"

type Subscriber struct {
	conn *Connection
}

func NewSubscriber(conn *Connection) *Subscriber {
	return &Subscriber{conn: conn}
}

func (s *Subscriber) Consume(ctx context.Context, handler func([]byte) error) error {
	msgs, err := s.conn.GetChannel().Consume(
		s.conn.queue,
		"",    // consumer
		false, // autoAck
		false, // exclusive
		false, // noLocal
		false, // noWait
		nil,   // args
	)
	if err != nil {
		return err
	}

	go func() {
		for {
			select {
			case <-ctx.Done():
				return
			case msg := <-msgs:
				if err := handler(msg.Body); err != nil {
					msg.Nack(false, true)
				} else {
					msg.Ack(false)
				}
			}
		}
	}()

	return nil
}
