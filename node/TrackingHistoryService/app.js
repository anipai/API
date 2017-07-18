var express = require("express"),
    https = require("https"),
    util = require("util"),
    app = express();
var request = require('request');
var f1 = require('./apiData.js');
var port = process.env.PORT || 3006;

app.get("/api/autherrors", function(request, response) {

    var uriSegment = request.query.uriSegment;
    var startTime = request.query.startTime;
    var endTime = request.query.endTime;
    var noOfRequests = request.query.noOfCalls;

    var date = new Date();
    if (date.getSeconds() % 2 != 0 && date.getSeconds() % 3 != 0 && date.getSeconds() % 7 != 0) {
    //if (uriSegment.length !== 0 && uriSegment.indexOf("signature") < 0) {
        response.status(204);
        response.end();
    } else {
        response.status(200);
    
        response.setHeader('Content-Type', 'application/json');
        var apiName = util.format('%s%s%s', 'tracking-v2-rest/', 'signature/', 'alert');
        var json = JSON.stringify([
            {
                "noOfCalls": "45",
                "apiName": apiName,
                "orgName": "Pitney-Bowes"
            },
            {
                "noOfCalls": "21",
                "apiName": apiName,
                "orgName": "Skypax Ltd"
            },
            {
                "noOfCalls": "35",
                "apiName": "tracking-v2-rest/signature",
                "orgName": "CCS"
            },
            {
                "noOfCalls": "28",
                "apiName": "tracking-v2-rest/signature",
                "orgName": "ReplySquad"
            }
        ]);
        response.end(json);
    }
});

app.get("/api/autherrors1", function(req, response) {
    f1.getDataFromIBM(req, (body) => {
        
            console.log(body);
            //response.send(body);
        
    });
    //console.log('datalog: ' + data);
    //response.send(data);
});

app.listen(port);
console.log("Listening on port ", port);
