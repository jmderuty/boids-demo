var urlParams;
(window.onpopstate = function () {
	var match,
	pl = /\+/g,  // Regex for replacing addition symbol with a space
	search = /([^&=]+)=?([^&]*)/g,
	decode = function(s) { return decodeURIComponent(s.replace(pl, " ")); },
	query = window.location.search.substring(1);

	urlParams = {};
	while (match = search.exec(query))
	{
		urlParams[decode(match[1])] = decode(match[2]);
	}
})();
var splitted = window.location.href.split('/');
var accountId = splitted[3];
var applicationName = splitted[4];
var adminPluginId = splitted[7];
var xToken = urlParams["x-token"];
document.cookie = "x-token="+encodeURIComponent(xToken);
