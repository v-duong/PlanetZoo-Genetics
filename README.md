# Planet Zoo Genetics Assitant
A tool designed to help you manage your breeding in Planet Zoo.

### Features
- Reads data from your zoo save (Only Sandbox and Franchise at the moment)
- Lists all animals currently in the zoo (and storage maybe)
- Calculate possible offspring between parents and their approximate probabilities

### Things to do
- Clean up offspring calculation code
- Improve UI, add better sorting and add filtering
- Add support for all save types
- Improve accuracy of data extraction
- Figure out mutation probabilities if possible

### Usage Tips
- Load your save file using File->Open in the top menu.
    -- Saves are located at Users\\<Username>\Saved Games\Frontier Developments\Planet Zoo\\<steamID>\Saves
    -- Franchise saves are some string of letter and numbers.
    -- **Backup your saves** before anything. This program does not write to the save but always be careful.
- Try to not calculate all possible breeding pairs if you can. You can disable the Breedable checkbox to exclude it from calculations.
- Genetic mutations are currently not taken into account. Thus, the probabilities are **not** accurate. They are only there to give a rough idea of the outcomes.
- Names are not always found properly. Use your best guess with the animal's stats, the genealogy window in game, and process of elimination if this occurs.

### Notes
- Saves are actually a zip file that contains four files. *parkdata* is the file that contains the zoo data. 