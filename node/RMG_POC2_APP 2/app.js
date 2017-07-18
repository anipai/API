/*jshint esversion: 6 */
var express = require('express');
var path = require('path');
var url = require('url');
var printf = require('printf')
var cookieParser = require('cookie-parser');
var bodyParser = require('body-parser');
var f = require('./functions.js');
var index = require('./routes/index');
var config = require('./config.json');
var port = 4001;

var app = express();

app.set('views', path.join(__dirname, 'views'));
app.set('view engine', 'jade');

app.use('/', index);
app.use(cookieParser());
app.use(express.static(path.join(__dirname, 'public')));
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({
	extended: false
}));
app.use(function (err, req, res, next) {
	res.locals.message = err.message;
	res.locals.error = req.app.get('env') === 'development' ? err : {};
	res.status(err.status || 500);
	res.render('error');
});

app.get('/ebusinesstoken', (req, res) => {
	var userId;
	if(req.query.userId) userId = req.query.userId;
	else userId = "";
	f.generateJWT(userId, (err, body) => {
		if(err) {
			console.log(err);
			res.send(err);
		} else {
			console.log(body);
			res.send(body);
		}
	});
});


app.get('/api/autherrors1', function(req, res) {

	console.log('entered');
    var getHeaders = {
        'Content-Type' : 'application/json',
        'authorization' : 'Basic YXBpbWFuYWdlci9hdmlrLnNlbmd1cHRhQHJveWFsbWFpbC5jb206VzFlbGNvbWUx'
    };

   
    var url = "https://eu.apiconnect.ibmcloud.com/v1/orgs/55a900b10cf272a4b3015a7a/environments/55f16d010cf2fae1b6b74fec/events?before=2016-12-20T14:59:59&after=2016-12-15T14:00:00&limit=200";

    request.get({
        url: url,
        headers: getHeaders,
        strictSSL: false
    }, (err, resp, body) => {
        if (err) {
            console.log(err);
            resp.send(err);
        } else {
            console.log(body);
            resp.send(body);
        }

    });
});

app.get('/getTrackingSummary', (req, res) => {
	var page = url.parse(req.url, true);
    var query = page.query;
	console.log(query);
	var trackingNumber = query.trackingNumber;
	var capchaToken = query["g-recaptcha-response"];
	var ibmJwtToken = query.jwtibm;
	var jwtcaptchasession = query.jwtcaptchasession;
	var apiUrl = config.api_url + trackingNumber + "&g-recaptcha-response=" + capchaToken + "&jwtibm=" + ibmJwtToken + "&jwtcaptchasession=" + jwtcaptchasession;
	console.log("apiUrl: " + apiUrl );
	f.getTrackingDetails(apiUrl, (err, body) => {
		if(err) {
			console.log(err);
			res.send(err);
		} else {
			console.log(body);
			res.send(body);
		}
	});


	
});

app.post('/html/captcha.html', (req, res) => {
	var token = req.body["g-recaptcha-response"];

	var ip = req.ip;
	f.postToGoogleCaptcha(ip, token, (err, googleResponse) => {
		if(err) {
			console.log(err);
			res.send(err);
		} else {
			console.log(googleResponse);
			res.send(googleResponse);
		}
	});
});

app.use(function (req, res, next) {
	var err = new Error('Not Found');
	err.status = 404;
	next(err);
});

app.listen(port, function () {
	console.log('listening on port ' + port);
});

module.exports = app;
