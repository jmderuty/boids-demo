var LeaderboardAPI = {
	get: function(xToken, id, skip, take) {
		var urlEnd = "";
		if (id)
		{
			urlEnd += "/"+id+"/"+skip+"/"+take;
		}
		return $.ajax("https://api.stormancer.com/"+accountId+"/"+applicationName+"/_admin/leaderboard"+urlEnd, {
			type: 'GET',
			contentType: "application/json",
			dataType: "json",
			headers: {
				"x-token": "eyJFeHBpcmF0aW9uIjoiMjAxNS0wOC0wMVQxNDozNzoyMC40NDQzMDc3WiIsIklzc3VlZCI6IjIwMTUtMDctMzFUMTQ6Mzc6MjAuNDQ0MzA3N1oifQ%3D%3D.ZBcDMHmWZis8eR7icoG78qEh9qOXyzvoXagjUKOvdnc%3D",
				"x-version": "1.0"
			}
		});
	}
};
