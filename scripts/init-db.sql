CREATE TABLE IF NOT EXISTS user_orders (
    id SERIAL PRIMARY KEY,
    order_id BIGINT NOT NULL,
    user_id BIGINT NOT NULL,
    category VARCHAR(50) NOT NULL,
    amount INT NOT NULL,
    unit_price DECIMAL(10,2) NOT NULL,
    total_price DECIMAL(10,2) GENERATED ALWAYS AS (amount * unit_price) STORED,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(user_id, order_id)
);

CREATE INDEX idx_user_orders_user_id ON user_orders(user_id);
CREATE INDEX idx_user_orders_created_at ON user_orders(created_at);