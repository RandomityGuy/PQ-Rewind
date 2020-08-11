
function Rewind::onConnected(%this)
{
	echo("Connected to the rewind server");
}

function getWordDelim(%str,%delim,%index)
{
	%newstr = strreplace(%str,%delim," ");
	return getWord(%newstr,%index);
}
function removeWordDelim(%str,%delim,%index)
{
	%newstr = strreplace(%str,%delim," ");
	%ret = removeWord(%newstr,%index);
	return strreplace(%ret," ",%delim);
}


function GetTTStates(%group)
{
	for (%i = 0; %i < %group.getCount(); %i++)
	{
		%object = %group.getObject(%i);
		%type = %object.getClassName();
		if (%type $= "SimGroup")
		{
			%procstate = %procstate @ GetTTStates(%object);
		}
		else
		{
			if (%object.dataBlock $= "TimeTravelItem")
			{
				%procstate = %procstate @ %object.isHidden() @ ",";
			}
		}
	}
	return %procstate;
}

function SetTTStates(%group,%state)
{
	$TTStateStack = %state;
	for (%i = 0; %i < %group.getCount(); %i++)
	{
		%object = %group.getObject(%i);
		%type = %object.getClassName();
		if (%type $= "SimGroup")
			SetTTStates(%object,$TTStateStack);
		else
			if (%object.dataBlock $= "TimeTravelItem")
			{
				%state = getWordDelim($TTStateStack,",",0);
				%object.hide(%state);
				$TTStateStack = removeWordDelim($TTStateStack,",",0);
				//%procstate = %procstate @ %object.isHidden() @ ",";
			}
	}
}


function GetGemStates(%group)
{
	for (%i = 0; %i < %group.getCount(); %i++)
	{
		%object = %group.getObject(%i);
		%type = %object.getClassName();
		if (%type $= "SimGroup")
			%procstate = %procstate @ GetGemStates(%object);
		else
			if ((%type $= "Item") && (%object.getDataBlock().className $= "Gem"))
			{
				%procstate = %procstate @ %object.isHidden() @ ",";
			}
	}
	return %procstate;
}

function SetGemStates(%group,%state)
{
	$GemStateStack = %state;
	for (%i = 0; %i < %group.getCount(); %i++)
	{
		%object = %group.getObject(%i);
		%type = %object.getClassName();
		if (%type $= "SimGroup")
			SetGemStates(%object,$GemStateStack);
		else
			if ((%type $= "Item") && (%object.getDataBlock().className $= "Gem"))
			{
				%state = getWordDelim($GemStateStack,",",0);
				%object.hide(%state);
				$GemStateStack = removeWordDelim($GemStateStack,",",0);
				//%procstate = %procstate @ %object.isHidden() @ ",";
			}
	}
}


function GetMPStates(%group)
{
	for (%i = 0; %i < %group.getCount(); %i++)
	{
		%object = %group.getObject(%i);
		%type = %object.getClassName();
		if (%type $= "SimGroup")
			%procstate = %procstate @ GetMPStates(%object);
		else
			if (%type $= "PathedInterior")
			{
				%procstate = %procstate @ %object.getPathPosition() @ ",";
				%procstate = %procstate @ %object.getTargetPosition() @ ";";
			}
	}
//echo (%procstate);
	return %procstate;
}
function SetMPStates(%group,%state)
{
	$MPStateStack = %state;
	echo("SETTING MP STATES");
	for (%i = 0; %i < %group.getCount(); %i++)
	{
		%object = %group.getObject(%i);
		%type = %object.getClassName();
		if (%type $= "SimGroup")
			SetMPStates(%object,$MPStateStack);
		else
			if (%type $= "PathedInterior")
			{
				%mpState = getWordDelim($MPStateStack,";",0);
				%object.setPathPosition(getWordDelim(%mpState,",",0));
				%object.setTargetPosition(getWordDelim(%mpState,",",1));
				$MPStateStack = removeWordDelim($MPStateStack,";",0);
				echo("MPSTATE" SPC %mpState);
				echo("CURSTATE"SPC %i SPC $MPStateStack);
			}
		}
	
}

function Rewind::onLine(%this, %line)
{
   RewindFrame(%line);
   echo("Popped Rewind Frame");
}

function RewindFrame(%frame)
{
	LocalClientConnection.isOOB = 0;
	LocalClientConnection.Player.setOOB(0);
	commandToClient(LocalClientConnection, 'LockPowerup', 0);
	cancel(LocalClientConnection.respawnSchedule);
	
	
	%framedata = getWords(%frame,1,17);
	$Time::CurrentTime = getWord(%framedata,0);
	echo(%framedata);
	if (strcmp(%frame,"FRAME 0 0 0 0 0 0 0 0 0 0 0 none 0 [] 0 [] []")==0)
	{
		echo("Stopped Rewinding");
		$rewinding=0;
		return;
	}
	MPGetMyMarble().setTransform(getWord(%framedata,2) SPC getWord(%framedata,3) SPC getWord(%framedata,4) SPC "1 0 0 0");
	MPGetMyMarble().setVelocity(getWord(%framedata,5) SPC getWord(%framedata,6) SPC getWord(%framedata,7));
	MPGetMyMarble().setAngularVelocity(getWord(%framedata,8) SPC getWord(%framedata,9) SPC getWord(%framedata,10));
	
	$Rewind::TimeBonus = getWord(%framedata,12);
	//localclientconnction.incBonusTime(-PlayGui.bonusTime);
	//PlayGui.bonusTime = 0;
	//MPGetMyMarble().client.incBonusTime($Rewind::TimeBonus);
	//PlayGui.bonusTime = $Rewind::TimeBonus;
	
	//time::setBonusTime(getWord(%framedata,12));
	%MPStates = stripChars(getWord(%framedata,13),"[]");
	SetMPStates(MissionGroup,%MPStates);
	
	%GemStates = stripChars(getWord(%framedata,15),"[]");
	SetGemStates(MissionGroup,%GemStates);
	
	%TTStates = stripChars(getWord(%framedata,16),"[]");
	SetTTStates(MissionGroup,%TTStates);
	
	localclientconnection.gemCount = getWord(%framedata,14);
	localclientconnection.setGemCount(localclientconnection.getGemCount());
	
	%powerup = getWord(%framedata,11);
	if (%powerup $= "none")
	{
		if (LocalClientConnection.player.getPowerup() != 0 && !(LocalClientConnection.player.getPowerup() $= ""))
		{
		MPGetMyMarble().setPowerup(0); 
		}
	}
	else
	{
		MPGetMyMarble().setPowerup(%powerup);
	}
	echo ("Frame: (" SPC getWord(%framedata,2) SPC getWord(%framedata,3) SPC getWord(%framedata,4) @ ")" SPC "(" @ getWord(%framedata,5) SPC getWord(%framedata,6) SPC getWord(%framedata,7) @ ")" SPC "(" @ getWord(%framedata,8) SPC getWord(%framedata,9) SPC getWord(%framedata,10) @ ")" SPC getWord(%framedata,11) SPC getWord(%framedata,12) SPC %MPStates SPC %TTStates);
	echo ("TIME BONUS" SPC $Rewind::TimeBonus);
}

function serverCbOnMissionLoaded() 
{
	$rewindTCP = new TCPObject(Rewind);
	$rewindTCP.connect("localhost:28005");
	moveMap.pop();
	moveMap.bind(keyboard,"r",sendRewindRequest);
	moveMap.push();
	
}

function sendRewindRequest(%val)
{
	if (%val)
	{
		$rewindTCP.send("popFrame 0\n");
		$rewinding = 1;
		echo("REWINDING");
	}
	if (%val == 0)
	{
		echo("STOPPED REWINDING");
		if ($Rewind::TimeBonus == 0)
		{
			//Localclientconnection.incBonusTime(-PlayGui.bonusTime);
			//PlayGui.bonusTime = 0;
			echo("BONUS TIME ZERO");
			time::start();
		}
		else
		{
			time::start();
			//Localclientconnection.incBonusTime(-PlayGui.bonusTime);
			//PlayGui.bonusTime = 0;
			Localclientconnection.incBonusTime($Rewind::TimeBonus);
			echo("REWIND TB" SPC $Rewind::TimeBonus);
			//time::addBonusTime($Rewind::TimeBonus);
			//PlayGui.bonusTime = $Rewind::TimeBonus;
		}
		$rewinding = 0;
	}
}

function serverCbOnFrameAdvance(%delta) 
{
	if ($rewinding == 0)
	{
		if (isObject(MPGetMyMarble()))
		{
			%powerup = "none";
			if (LocalClientConnection.player.getPowerup() != 0 && !(LocalClientConnection.player.getPowerup() $= ""))
			{
				%powerup = LocalClientConnection.player.getPowerup();
			}
			echo("Sending Frame");
			$rewindTCP.send("pushFrame" SPC $Time::CurrentTime SPC %delta SPC MPGetMyMarble().getPosition() SPC MPGetMyMarble().getVelocity() SPC MPGetMyMarble().getAngularVelocity() SPC %powerup SPC PlayGui.bonusTime SPC "[" @ GetMPStates(MissionGroup) @ "]" SPC localclientconnection.getGemCount() SPC "[" @ GetGemStates(MissionGroup) @ "]" SPC "[" @ GetTTStates(MissionGroup) @ "]" @ "\n");
		}
	}
	if ($rewinding == 1)
	{
		time::start();
		localclientconnection.incBonusTime(-PlayGui.bonusTime);
		time::stop();
		sendRewindRequest(1);
	}
}
function serverCbOnMissionReset() 
{
	$rewindTCP.send("clearFrames 0\n");
	$rewinding = 0;
}
function serverCbOnMissionEnded() 
{
	moveMap.pop();
	moveMap.unbind("keyboard","r");
	moveMap.push();
}