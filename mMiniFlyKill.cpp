// Include the Object Set Header
#include "mMiniFlyKill.h"
#include "mArea.h"
#include "mImage.h"
#include "mStateButton.h"

// START COUNTER TIME (ms)
#define COUNTER_TIME    (5000)

// Constructor
CmMiniFlyKill::CmMiniFlyKill()
{
	this->m_dead_flies = 0;
	this->m_show_end_message = true;
	this->m_game_done = false;
	this->m_player_lost = false;
	this->m_player_stop = false;
	// Temporary pointer variable to Mini Game Button
	CmStateButton* tmpSet;
	for(int i = 0; i < 8; i++)
	{
		// Cretae the button
		tmpSet = new CmStateButton();

		if(i == 0)
		{
			// Add the image needed for the game
			tmpSet->Add(new CmImage(CIwSVec2(0, 0),"BG_game_1"));
		}
		else if(i == 1)
		{
			// Add the image needed for the game
			tmpSet->Add(new CmImage(CIwSVec2(212, 2),"BG_game_1_asset_1"));
		}
		else
		{
			int number = rand()%600; 
			// Add the image needed for the game
			tmpSet->m_revealed = false;
			tmpSet->Add(new CmImage(CIwSVec2(number+100, i*40),"BG_game_1_asset_2"));
			tmpSet->Add(new CmImage(CIwSVec2(number+100, i*40),"BG_game_1_asset_3"));
		}
		// Add this button 
		this->Add(tmpSet);
		// Reset the pointer
		tmpSet = NULL;
	}

	// Font Initialization
	this->m_counterMs = COUNTER_TIME;
	this->m_counterText = new CmFont(CIwSVec2(10,12),"strato_12", "");
	this->Add(this->m_counterText);
}
// Destructor
CmMiniFlyKill::~CmMiniFlyKill(){} 

// Render
void CmMiniFlyKill::Render(uint16 deltaMs)
{
	// Temporary pointer variable to State Button
	CmStateButton* tmpSet = NULL;

	char str[128];

	if(m_game_done == false)
	{
		if(this->m_counterMs > 0)
		{
			if(this->m_counterMs > deltaMs)
			  this->m_counterMs -= deltaMs;
			else
			{
				this->m_player_lost = true;
			}

			sprintf(str,"Time Left : %i", this->m_counterMs / 1000);
			this->m_counterText->SetText(str);
		}
	}

	//This can be used if you (like in the tutorial) want to use more buttons in your minigame
	
	for(int i = 0; i < 6; i++)
	{
		if(m_game_done == false)
		{
			tmpSet = (CmStateButton*) m_objects[i+2];

			if(((CmArea*) ((*tmpSet)[0]))->IsReleased())
			{
				if(tmpSet->m_revealed == false)
				{
					m_dead_flies++;
					tmpSet->m_revealed = true;
					tmpSet->Next();
				}
			}
		}
	}

	if(m_dead_flies == 6)
	{
		this->FinishGame("win_screen", true, deltaMs);
	}

	if(this->m_player_lost == true)
	{
		this->FinishGame("lose_screen", false, deltaMs);
	}

	CmObjectSet::Render(deltaMs);
}

void CmMiniFlyKill::FinishGame(const char* image_name, bool playerWin, uint16 deltaMs)
{
	CmStateButton* tmpSet = NULL;

	this->m_game_done = true;

	if(m_show_end_message == true)
	{
		//Set counter time
		this->m_counterMs_end = 500;

		//Unload the maze
		this->Unload();

		//Create a button
		CmStateButton* tmpBut;
		//Create a state button
		tmpBut = new CmStateButton();
		//Add the image to the button
		tmpBut->Add(new CmImage(CIwSVec2(0, 0),image_name));
		//Add the image to the maze
		this->Add(tmpBut);
		// Load The Image
		this->Load();

		m_show_end_message = false;
	}
	//Retrieve the created button
	tmpSet = (CmStateButton*) m_objects[0];

	//Build timer to let the player press the stop button
	if(this->m_counterMs_end > 0)
	{
		if(this->m_counterMs_end > deltaMs)
			this->m_counterMs_end -= deltaMs;
		else
		{
			this->m_player_stop = true;
		}
	}

	if(this->m_player_stop == true)
	{
		//Check if the ending screen is pushed
		if(((CmArea*) ((*tmpSet)[0]))->IsPressed())
		{
			//If the player is done with the minigame do this
			maze->m_play_miniGame = false;
			//Check if the player has won the minigame
			maze->m_won_minigame = playerWin;
		}	
	}
}
