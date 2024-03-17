CREATE TABLE "{schema}"."product" (
    id serial PRIMARY KEY,
    product_json jsonb NOT NULL,
    created timestamp with time zone NOT NULL,
    updated timestamp with time zone NOT NULL
);