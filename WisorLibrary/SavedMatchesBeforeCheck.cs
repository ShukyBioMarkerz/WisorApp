using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WisorLib
{
    class OneSavedMatchBeforeCheck
    {
        // General Parameters
        public Option[] savedMatch = { null, null };


        public OneSavedMatchBeforeCheck(Option[] pointForSave)
        {
            savedMatch = pointForSave;
        }
    }





    class SavedMatchesBeforeCheck
    {
        // General Parameters
        public List<OneSavedMatchBeforeCheck> savedMatchesList = null;
        public int numOfMatches = 0;


        // Saving matching point
        private Option[] pointForSave = { null, null };



        public SavedMatchesBeforeCheck()
        {
            savedMatchesList = new List<OneSavedMatchBeforeCheck>();
        }



        // **************************************************************************************************************************** //
        // ************************************************* Insert Single Match To List ********************************************** //

        public void InsertMatchToList(Option matchingPointOptX, Option matchingPointOptY)        
        {
            pointForSave[(int)Options.options.OPTX] = matchingPointOptX;
            pointForSave[(int)Options.options.OPTY] = matchingPointOptY;
            OneSavedMatchBeforeCheck matchForSave = new OneSavedMatchBeforeCheck(pointForSave);
            savedMatchesList.Add(matchForSave);
            numOfMatches++;
        }


        // **************************************************************************************************************************** //
        // ********************************************** Insert List of Matches To List ********************************************** //

        public void InsertListOfMatches(SavedMatchesBeforeCheck listToAdd)
        {
            for (int i = 0; i < listToAdd.numOfMatches; i++)
            {
                savedMatchesList.Add(listToAdd.savedMatchesList[i]);
                numOfMatches++;
            }
        }

    }
}
