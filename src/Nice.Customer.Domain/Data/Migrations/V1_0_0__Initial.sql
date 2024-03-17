CREATE TABLE "{schema}"."customer" (
    id serial PRIMARY KEY,
    customer_json jsonb NOT NULL,
    created timestamp with time zone NOT NULL,
    updated timestamp with time zone NOT NULL
);