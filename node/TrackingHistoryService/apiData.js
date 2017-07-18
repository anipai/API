var request = require('request');
var config = require('./config.json');
var util = require("util");
var f = module.exports = {};
var async = require("async");
f.getDataFromIBM = function (req, ibmCallback) { // replace params here
	// code goes here

	var uriSegment = req.query.uriSegment;
    var startTime = req.query.startTime;
    var endTime = req.query.endTime;
	var config = require('./config.json');
	var noOfRequests = typeof req.query.noOfCalls != "undefined" ? req.query.noOfCalls : config.tolerance;   
	if(!uriSegment)
	{
		return 'no UriSegment found';
	}
	if(!startTime)
	{
		return 'no Start Time found';
	}
	if(!uriSegment)
	{
		return 'no End Time found';
	}
	
    var userName = config.userName, 
        password = config.password,
        baseUrl = config.baseUrl,
        pathFormat = config.path,
        limit = config.limit,
        env = config.environment,
        org = config.org
    
    var encodedCredentials = new Buffer(util.format('%s:%s', userName, password)).toString('base64');

    var path = util.format(pathFormat, org, env, endTime, startTime, limit);
    var url = baseUrl + path; //;

    console.log("url ", url);
    console.log("path ", path);
    console.log("encodedCredentials ", util.format('Basic %s', encodedCredentials));

    var getHeaders = {
        'Content-Type' : 'application/json',
        'authorization' : util.format('Basic %s', encodedCredentials)//'Basic YXBpbWFuYWdlci9hdmlrLnNlbmd1cHRhQHJveWFsbWFpbC5jb206VzFlbGNvbWUx'
    };

    //var url = "https://eu.apiconnect.ibmcloud.com/v1/orgs/55a900b10cf272a4b3015a7a/environments/55f16d010cf2fae1b6b74fec/events?before=2017-01-20T14:59:59&after=2017-01-20T14:00:00&limit=10";

    var calls = "{}";
    var count = 0;
    var total = 0;
    //import whilst from 'async/whilst';

    //---test

    async.whilst(
    function() { return count < 5; },
    function(callback) {
        count++;
        
        request.get({
        url: url,
        headers: getHeaders,
        strictSSL: false
    }, (err, res, body) => {
        if(err)
        {
            console.log('err: ' + err );
            return ibmCallback(err);
        }
        console.log('count ' + count)
        console.log(body);
        
        callback(null, count);
        
    });
    },
    function (err, n) {
        console.log(calls);
        return ibmCallback(calls);
    }
);
    //---test end

    

    //return callback(calls);
	//return
};