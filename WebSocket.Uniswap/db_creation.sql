DROP DATABASE IF EXISTS candleswap;
GO
CREATE DATABASE candleswap;
GO
USE candleswap;

CREATE TABLE candles(
    id BIGINT NOT NULL IDENTITY(1, 1) PRIMARY KEY,
    datetime DATETIME NOT NULL,
    resolutionSeconds INT NOT NULL,
    token0Id VARCHAR(255) NOT NULL,
    token1Id VARCHAR(255) NOT NULL,
    [open] VARCHAR(78) NOT NULL,
    high VARCHAR(78) NOT NULL,
    low VARCHAR(78) NOT NULL,
    [close] VARCHAR(78) NOT NULL,
    volume INTEGER NOT NULL
);

ALTER TABLE candles
ADD CONSTRAINT UC_candles UNIQUE(token0Id, token1Id, datetime, resolutionSeconds);