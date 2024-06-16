-- criação do banco de dados
CREATE DATABASE system;

-- conexão do banco de dados
\c EventosMapa;

-- tabela de eventos (clientes)
CREATE TABLE system (
    Id SERIAL PRIMARY KEY,
    Endereco VARCHAR(255) NOT NULL,
    NomeEvento VARCHAR(100) NOT NULL,
    DescricaoEvento TEXT,
    Segmentos VARCHAR(100),
    Latitude DOUBLE PRECISION NOT NULL,
    Longitude DOUBLE PRECISION NOT NULL,
    DescricaoLocal TEXT
);
