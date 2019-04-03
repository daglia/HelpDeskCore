
// Homepage-2.js
// ====================================================================
// This file should not be included in your project.
// This is just a sample how to initialize plugins or components.
//
// - Designbudy.com -


$(document).ready(function() {


	// SITE STATISTICS
    // =================================================================
    // Require flot Spine Charts
    // -----------------------------------------------------------------
    // http://www.flotcharts.org/
    // =================================================================

    var origData = {
            daterange: ["7-1-16", "7-7-16"],
            count: [100, 500, 300, 900, 300, 500, 300]
        },
        ticks = [],
        max = 0;

    function formatData(data) {
        dataTemp = {
            date: [],
            count: data.count
        };
        dataOut = [];

        var range = moment().range(moment(data.daterange[0], "M-D-YY"), moment(data.daterange[1], "M-D-YY"));

        range.by(moment().range(moment(data.daterange[0], "M-D-YY"), moment(data.daterange[0], "M-D-YY").add("days", 1)), function(m) {
            dataTemp.date.push(m.valueOf());
            ticks.push([m.valueOf(), m.format("MMM D")]);
        });

        num = dataTemp.count.length;

        for (var i = 0; i < num; i++) {
            dataOut.push([dataTemp.date[i], dataTemp.count[i]]);
        }

        var units = Math.pow(10, Math.floor(Math.log(Math.max.apply(Math, dataTemp.count)) / Math.log(10)));
        max = Math.ceil(Math.max.apply(Math, dataTemp.count) / units) * units;

        return dataOut;
    }

    var plot = $.plot("#demo-site-statistics", [{
        label: "New Visitors",
        data: formatData(origData)
    }], {
        series: {
            points: {
                radius: 5,
                fill: true,
                show: true
            },
            splines: {
                show: true,
                tension: 0.4,
                lineWidth: 2,
				fill: true
            },
        },
        xaxis: {
            tickLength: 0,
            ticks: ticks,
        },
        colors: ["#666666"],
        shadowSize: 0,
        tooltip: true,
        tooltipOpts: {
            content: function(label, x, y) {
                return '<div class="hover-title">' + moment(x).format("dddd, MMMM Do YYYY") + '</div><b style="color:#e74c3c">' + y.toLocaleString() + " </b><span>" + label.toLowerCase() + "</span>";
            }
        },
        grid: {
            hoverable: true,
            borderWidth: 0,
            margin: 1,
            mouseActiveRadius: 2000
        },
    });

	// ORDER STATISTICS
    // =================================================================
    // Require flot Spine Charts
    // -----------------------------------------------------------------
    // http://www.flotcharts.org/
    // =================================================================

    var origData = {
            daterange: ["7-1-16", "7-7-16"],
            count: [100, 80, 140, 100, 80, 160, 180]
        },
        ticks = [],
        max = 0;

    function formatData(data) {
        dataTemp = {
            date: [],
            count: data.count
        };
        dataOut = [];

        var range = moment().range(moment(data.daterange[0], "M-D-YY"), moment(data.daterange[1], "M-D-YY"));

        range.by(moment().range(moment(data.daterange[0], "M-D-YY"), moment(data.daterange[0], "M-D-YY").add("days", 1)), function(m) {
            dataTemp.date.push(m.valueOf());
            ticks.push([m.valueOf(), m.format("MMM D")]);
        });

        num = dataTemp.count.length;

        for (var i = 0; i < num; i++) {
            dataOut.push([dataTemp.date[i], dataTemp.count[i]]);
        }

        var units = Math.pow(10, Math.floor(Math.log(Math.max.apply(Math, dataTemp.count)) / Math.log(10)));
        max = Math.ceil(Math.max.apply(Math, dataTemp.count) / units) * units;

        return dataOut;
    }

    var plot = $.plot("#demo-sales-statistics", [{
        label: "New Sales",
        data: formatData(origData)
    }], {
        series: {
            points: {
                radius: 5,
                fill: true,
                show: true
            },
            splines: {
                show: true,
                tension: 0.4,
                lineWidth: 2,
				fill: true
            },
        },
        xaxis: {
            tickLength: 0,
            ticks: ticks,
        },
        colors: ["#e74c3c"],
        shadowSize: 0,
        tooltip: true,
        tooltipOpts: {
            content: function(label, x, y) {
                return '<div class="hover-title">' + moment(x).format("dddd, MMMM Do YYYY") + '</div><b style="color:#e74c3c">' + y.toLocaleString() + " </b><span>" + label.toLowerCase() + "</span>";
            }
        },
        grid: {
            hoverable: true,
            borderWidth: 0,
            margin: 1,
            mouseActiveRadius: 2000
        },
    });

	
    // FLOT BAR CHART - NEGATIVE
    // =================================================================
    // Require flot Charts
    // -----------------------------------------------------------------
    // http://www.flotcharts.org/
    // =================================================================

    var negativebar = [
        [0, 0],
        [1, 0.8],
        [2, 0.9],
        [3, 0.1],
        [4, -0.8],
        [5, -1.0],
        [6, -0.3],
        [7, 0.7],
        [8, 1],
        [9, 0.4],
        [10, -0.5],
        [11, -1],
        [12, -0.5],
        [13, 0.4],
        [14, 1],
        [15, 0.7],
        [16, -0.3],
        [17, -1.0],
        [18, -0.8],
        [19, 0.1],
    ];

    var data = [{
        label: "value A",
        data: negativebar
    }, ];

    $.plot($("#demo-negativebar"), data, {
        series: {
            bars: {
                show: true,
                barWidth: 0.5,
                horizontal: false,
                order: 1,
                fillColor: {
                    colors: [{
                        opacity: 0.5
                    }, {
                        opacity: 0.9
                    }]
                }
            }
        },
        legend: {
            show: true
        },
        grid: {
            borderWidth: 1,
            tickColor: "#eeeeee",
            borderColor: "#eeeeee",
            hoverable: true,
            clickable: true
        },
        xaxis: {
            font: {
                color: "#ccc"
            }
        },
        yaxis: {
            font: {
                color: "#ccc"
            }
        },
        colors: ['#29b7d3'],
    });


// FLOT REAL TIME CHART
// =================================================================
// Require Flot Area Chart
// -----------------------------------------------------------------
// http://www.flotcharts.org/
// =================================================================


        var data1 = [];
        var totalPoints = 300;
        function GetData() {
        data1.shift();
        while (data1.length < totalPoints) {
        var prev = data1.length > 0 ? data1[data1.length - 1] : 50;
        var y = prev + Math.random() * 10 - 5;
        y = y < 0 ? 0 : (y > 100 ? 100 : y);
        data1.push(y);
        }
    var result = [];
    for (var i = 0; i < data1.length; ++i) {
        result.push([i, data1[i]])
        }
    return result;
    }
    var updateInterval = 100;
    var plot = $.plot($("#demo-realtime"), [
            GetData()], {
            series: {
                lines: {
                    show: true,
                    fill: true
                },
                shadowSize: 0
            },
            yaxis: {
                min: 0,
                max: 100,
                ticks: 10
            },
            xaxis: {
                show: true
            },
            grid: {
                hoverable: true,
                clickable: true,
                tickColor: "#eeeeee",
                borderWidth: 1,
                borderColor: "#eeeeee"
            },
            colors: ["#5abcdf", "#ff8673"],
            tooltip: true,
            tooltipOpts: {
                defaultTheme: false
            }
        });
        function update() {
            plot.setData([GetData()]);
            plot.draw();
            setTimeout(update, updateInterval);
        }
        update();
		
	// SUMMERNOTE
	// =================================================================
	// Require Summernote
	// http://hackerwins.github.io/summernote/
	// =================================================================
    $('#demo-summernote').summernote({
        toolbar: [
            ['style', ['bold', 'italic', 'underline', 'clear']],
            ['fontsize', ['fontsize']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['height', ['height']],
        ],
        height: 300   //set editable area's height
    });

});