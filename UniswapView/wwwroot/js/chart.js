function drawChart(candlesUri) {
    const chartProperties = {
        width: 1000,
        height: 500,
        timeScale: {
            timeVisible: true,
            secondsVisible: false
        }
    }
    const domElement = document.getElementById("chart_div");
    const chart = LightweightCharts.createChart(domElement, chartProperties);
    const candleSeries = chart.addCandlestickSeries();

    fetch(candlesUri)
        .then((res) => res.json())
        .then((data) => {
            const cdata = data.map((d) => {
                return {
                    time: Math.floor(Date.parse(d['datetime']) / 1000),
                    open: parseFloat(d['_open']),
                    close: parseFloat(d['close']),
                    high: parseFloat(d['high']),
                    low: parseFloat(d['low'])
                }
            }).reverse()
            console.log(cdata);
            candleSeries.setData(cdata);
        })
        .catch(err => console.log(err))
}