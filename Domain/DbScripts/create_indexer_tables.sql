USE candleswap;

CREATE TABLE blocks(
    number VARBINARY(32) PRIMARY KEY,
    timestamp VARBINARY(32) NOT NULL
);

CREATE TABLE transactions(
    hash VARBINARY(32) PRIMARY KEY,
    token0Id VARBINARY(20) NOT NULL,
    token1Id VARBINARY(20) NOT NULL,
    amountIn VARBINARY(32) NOT NULL,
    amountOut VARBINARY(32) NOT NULL,
    blockNumber VARBINARY(32) NOT NULL FOREIGN KEY REFERENCES blocks(number),
    nonce VARBINARY(32) NOT NULL
);
/*
CREATE TABLE swapEvents(
    id BIGINT NOT NULL IDENTITY(1, 1) PRIMARY KEY,
    transactionHash VARBINARY(32) NOT NULL FOREIGN KEY REFERENCES transactions(hash),
    amount0In VARBINARY(32) NOT NULL,
    amount1In VARBINARY(32) NOT NULL,
    amount0Out VARBINARY(32) NOT NULL,
    amount1Out VARBINARY(32) NOT NULL
);

ALTER TABLE swapEvents
ADD CONSTRAINT UC_swapEvents UNIQUE(transactionHash, amount0In, amount1In, amount0Out, amount1Out);*/