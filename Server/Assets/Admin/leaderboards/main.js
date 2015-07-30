window.onload = main;

function main()
{
	leaderboardGlobalVM = new LeaderboardGlobal();
	for (var i=0; i<3; i++)
	{
		leaderboardGlobalVM.leaderboards.push(new LeaderboardViewModel(parseInt(Math.random()*100)));
	}
	ko.applyBindings(leaderboardGlobalVM);
}
