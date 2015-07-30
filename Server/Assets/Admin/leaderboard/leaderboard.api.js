var LeaderboardAPI = {
	get: function(id, skip, take) {
		return $.get("https://api.stormancer.com/"+accountId+"/"+applicationName+"/_admin/leaderboard/"+(id?id:'')+(skip?"?skip="+skip:'')+(take?(skip?"&":"?")+"take="+take:''), {
			contentType: "application/json",
			dataType: "json",
			headers: {
				"x-token": xToken,
				"x-version": "1.0"
			}
		});
	}
};
