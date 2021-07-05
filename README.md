# Sideways

[![Build Status](https://johan-larsson.visualstudio.com/Sideways/_apis/build/status/JohanLarsson.Sideways?branchName=main)](https://johan-larsson.visualstudio.com/Sideways/_build/latest?definitionId=18&branchName=main)

App for backtesting and viewing charts.

![image](https://user-images.githubusercontent.com/1640096/119938653-0891d100-bf8d-11eb-80d2-1dd383fb06cf.png)

This is currently pre alpha so there may be breaking changes.

# Get started
1. Create an API key at [AlphaVantage](https://www.alphavantage.co/)
2. Build and run the app and open the settings editor and input the key.
3. Type in some symbol names. This starts downloads of days in the background.
4. Open the downloader to download minutes, this is very slow with the free version of AlphaVantage.


# Use
Type in symbols, end with enter. The first time a symbol is entered daily candles are downloaded in the background. The download can be very slow as it throttles to five downloads per minute. To download minues open the downloader and refresh the list then klick download.

Scroll to change time.
Arrow left and right moves time one day candle. Shift left and right one hour candle.
Space starts animation.
Ctrl + W adds current symbol to watchlist.

Alt + R selects random item in current list, symbols or bookmarks.
Alt + Left and Right moves back and forward in history.

## For non programmers
Currently we don't distribute binaries for download meaning sideways is built from source. Here are the steps:

1. Get the code
  - Install git if not installed https://gitforwindows.org/
  - Run `git clone "https://github.com/JohanLarsson/Sideways" "C:/git/Sideways"` in a command prompt

2. Build the code
  - Install x64 SDK from https://dotnet.microsoft.com/download/visual-studio-sdks
  - Click `C:/git/Sideways/run.cmd` it takes a couple of minutes as it downloads latest sources then builds them and runs the app. Nice ting is that it will always be the most recent version of sideways.

From now on start sideways by clicking run.cmd, it takes a while but is nice as it is automatic update.
