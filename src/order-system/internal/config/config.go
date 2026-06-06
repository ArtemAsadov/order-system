package config

type Config struct {
	Service  ServiceConfig  `yml:"service"`
	RabbitMQ RabbitMQConfig `yml:rabbitmq`
}

type ServiceConfig struct {
	Name string `yml:"name"`
	Port int    `yml:"port"`
}

type RabbitMQConfig struct {
	Host     string `yml:"host"`
	Port     int    `yml:"port"`
	User     string `yml:"user"`
	Password string `yml:"password"`
}
