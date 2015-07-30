function LeaderboardGlobal() {
	this.leaderboards = ko.observableArray();
	this.selectedLeaderboard = ko.observable();

	LeaderboardAPI.get().then(function(leaderboards) {
		for (var i=0; i<3; i++)
		{
			var leaderboard = leaderboards[i];
			leaderboardGlobalVM.leaderboards.push(new LeaderboardViewModel(leaderboard.id, leaderboard.name));
		}
	}).fail(function(e) {
		console.log("leaderboard API get error:", e);
	});
}

function LeaderboardViewModel(id, name)
{
	this.id = ko.observable(id || null);
	this.name = ko.observable(name || "");
	this.players = ko.observableArray();
	this.players.subscribe(function(newValue) {
		this.players().sort(sort2PlayersByScore);
	}.bind(this));

	for (var i=0;i<10;i++)
	{
		var id = parseInt(Math.random()*1000);
		this.players.push(new PlayerViewModel(id, "Player #"+id, parseInt(Math.random()*1000)));
	}
}

LeaderboardViewModel.prototype.select = function()
{
	leaderboardGlobalVM.selectedLeaderboard(this);
};

function PlayerViewModel(id, name, score)
{
	this.id = ko.observable(id || "");
	this.name = ko.observable(name || "");
	this.score = ko.observable(score || 0);
}

function sort2PlayersById(pvm1, pvm2)
{
	if (pvm1.id() > pvm2.id())
	{
		return true;
	}
	else
	{
		return false;
	}
}

function sort2PlayersByName(pvm1, pvm2)
{
	if (pvm1.name() > pvm2.name())
	{
		return true;
	}
	else
	{
		return false;
	}
}

function sort2PlayersByScore(pvm1, pvm2)
{
	if (pvm1.score() > pvm2.score())
	{
		return true;
	}
	else
	{
		return false;
	}
}
