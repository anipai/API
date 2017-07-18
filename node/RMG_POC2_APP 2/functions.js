/*jshint esversion: 6 */
var request = require('request');
var config = require('./config.json');
var f = module.exports = {};

f.postToGoogleCaptcha = function (ip, token, callback) {
	request.post({
		form: {
			secret: config.capcha_secret,
			response: token,
			remoteip: ip
		},
		url: config.google_captcha_url,
		headers: {
			"Content-Type": "application/json",
			"X-IBM-Client-Id": config.client_id
		},
		strictSSL: false
	}, (err, res, body) => {
		return callback(err, body);
	});
};

f.generateJWT = function (userid, callback) {
	var targetURL = config.generate_jwt_token_url;
	if(userid) targetURL += userid.toString();
	else targetURL = targetURL.split("?")[0];
	request.get({
		url: targetURL,
		headers: {
			"Content-Type": "application/json",
			"X-IBM-Client-Id": config.client_id
		},
		strictSSL: false
	}, (err, res, body) => {
		return callback(err, body);
	});
};

f.getTrackingDetails = function (url, callback) {
	
	request.get({
		url: url,
		headers: {
			"Content-Type": "application/json"
		},
		strictSSL: false
	}, (err, res, body) => {
		return callback(err, body);
	});
};

