CREATE TABLE "{schema}"."purchase_order" (
    id serial PRIMARY KEY,
    customer_id uuid GENERATED ALWAYS AS ((purchase_order_json ->> 'customer_id')::uuid) STORED,
    purchase_order_json jsonb NOT NULL,
    created timestamp with time zone NOT NULL,
    updated timestamp with time zone NOT NULL
);