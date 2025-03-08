# Veeam TestTask

This project synchronizes two folders periodically, ensuring that the replica folder matches the source folder. It checks for changes in files and directories and performs operations like copying, 
updating, and deleting.

## Requirements

- .NET Framework (or .NET Core)
- File system with access to the source and replica folders

## Usage

- Creates the replica folder if it doesn't exist.
- Synchronizes files and directories from the source folder to the replica folder.
- Runs periodically based on the sync interval (in seconds).
- Logs all operations (file and directory creation, deletion, and updates).

## Exit the program

To exit the program, either press Ctrl+C in the terminal or type q and hit Enter.

## License

This project is open source.


