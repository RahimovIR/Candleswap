google.charts.load('current', { 'packages': ['corechart'] });
google.charts.setOnLoadCallback(drawChart);

function drawChart(rawData) {
    console.log(rawData);
    var data = google.visualization.arrayToDataTable(rawData, true);
    
    var options = {
        legend: 'none'
    };

    var chart = new google.visualization.CandlestickChart(document.getElementById('chart_div'));

    chart.draw(data, options);
}