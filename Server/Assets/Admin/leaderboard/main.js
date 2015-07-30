window.onload = main;

function main()
{
	leaderboardGlobalVM = new LeaderboardGlobal();
	ko.applyBindings(leaderboardGlobalVM);
}
