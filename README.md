# Cryptocurrency tax return generator

> Fully automated offline tax return generator supporting accounts with an extremely high number of trades (millions of trades is ok).
> Calculation of capital gain for every trades. Binance is fully supported, Kucoin basics is supported as well.

> cryptocurrency, tax return, high frequency, binance, kucoin, capital gain, FIFO/LIFO/HIFO, USD, AUD

> .Net Core 3.1, Oxyplot

I created this software at the end of two years of trading. I'd gone to multiple companies who all charged in the thousands to calculate my tax - I figured there would be other people in the same position as me, so why not put it online for others? 

**What is included**

- Fully Free
- Offline - you keep the ownership of your trades history.
- Profile with more than 1 million trades supported. Such profiles in online paid tools will be extremely expensive to sort out.
- Multiple currencies: USD and AUD
- Multiple fee asset (BNB fee when trading on non BNB pairs)
- Binance specific event supported: Launchpad / Dust converter / Delisting of coins
- Flexible start of financial year
- Tax methodologies: LIFO, HIFO, FIFO
- Short-term / long-term profit or loss
- Multiple accounts

**A set of reports is also generated**

- Tax report per year
- Full detail of capital gain events
- Capital gain chart over time
- Capital gain per asset
- Capital gain per category (trade, fee, dividend, launchpad)
- End of financial years portfolio
- Current unrealized capital gain, with open cost history. Will track wich part of your unrealized will be on low tax.

## How to generate your tax report?

You will have to run those two steps

- Extract accounts data
- Generate tax report

### Extracting accounts data

This step will save locally your accounts info. The tax report generation will read those files and will be fully offline.

Set `PathsConfig.json` with custom values, or keep default.
```
{
  "apiKeyConfigPath": "C:\\temp\\ApiKey.txt",
  "extractFolder": "C:\\Taxes\\Extract",
  "reportGenerationFolder": "C:\\Taxes\\Report"
}
```

Run `Extract.PriceAndSymbols` project. It extracts prices history and trading pairs informations.

For each of your binance accounts/sub-accounts create an `ApiKey.txt` file with format (3 lines)
```
Key
Secret
UniqueAccountName
```

Then modify `PathsConfig.json` field `apiKeyConfigPath` to point to this file.
Run `Extract.Binance.AllOthers` project. 
It saves today's account balances, deposits, withdraws, dividends and dust conversions.
Run `Binance.Trades project`. This step can take up to 30 minutes for big accounts (1+ million trades) as binance got an api call limit.

For each of your kucoin accounts create an `ApiKey.txt` file with format (4 lines)
```
Key
Secret
UniqueAccountName
PassPhrase
```

Then modify `PathsConfig.json` field `apiKeyConfigPath` to point to this file.
Run `Extract.KuCoin` project.

Now your extracts are done, you can run CapitalGainGenerator.

### Generating tax report
You will have to start editing the file `Config.json` with your custom settings.
For an Australian tax return, the exchange rate csv file `AudUsd.csv` needs to be updated from [this official source](https://www.rba.gov.au/statistics/historical-data.html).
For an account with Delisting transactions, the file `YourHomework\Delisting.csv` needs to be filled.
The report will be accessible in the folder specified in `PathsConfig.json` field `reportGenerationFolder` .

#### Trouble shoot

> Timestamp for this request was 1000ms ahead of the server's time. 

You will have to resynchronize your operation system clock.  
  
> I have some portfolio breaches

You get portfolio breaches when your balance for any asset is going below zero.
It can happen if:
- you have missing trades (see below)
- your trades are not being run in the right order (see below)


> I have missing trades

Some trades cannot be extracted via API and should be extracted manually using [binance website](https://www.binance.com/en/my/orders/exchange/usertrade)

- [CTR trades](https://www.binance.com/en/support/articles/360002428872)
- [USDC/BTC trades](https://www.binance.com/en/support/articles/360020702932) has been introduced by binance in the wrong base/quote order. It started as USDC/BTC and then has been fixed as BTC/USDC. 
USDC/BTC trades should be extracted manually. BTC/USDC trades are fine.
When trades are manually extracted the timestamp prevision is in seconds. Api extract has a precision in milliseconds.
Manually extracted trade timestamp is in local time. Api extracted trade timestamp is UTC.

> My trades are not being run in the right order

In the case you have both manually extracted trades and API extracted, the order could be wrong.
Another case is when two trades happen at the exact same time, the timestamp cannot tell which of the two trades to play first.
You will have to modify your breaching trade time to get the right order.
Generated file `PortfolioBreaches.txt` can be used to discover which trade and how it should be modified.
Those cases happens very rarely, it is more likely they won't happen for your profile.
> The console shows the message "No funds origin for transfer"

You should add an entry in file `YourHomework\FundsOrigin.csv`

#### Wrap up
I hope this project will make it easy to fill your tax return.
Do not hesitate to raise any pull request for extraz features.