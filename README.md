# Sideways

[![Build Status](https://johan-larsson.visualstudio.com/Sideways/_apis/build/status/JohanLarsson.Sideways?branchName=main)](https://johan-larsson.visualstudio.com/Sideways/_build/latest?definitionId=18&branchName=main)

App for backtesting and viewing charts.

![image](https://user-images.githubusercontent.com/1640096/119938653-0891d100-bf8d-11eb-80d2-1dd383fb06cf.png)

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
