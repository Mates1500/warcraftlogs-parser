# How to use
1. Create `config.secret.json`
2. Fill with the following info:
```json
{
  "client": {
    "id": "CLIENT_ID",
    "secret": "CLIENT_SECRET"
  }
}
```
Get your client id and secret at https://www.warcraftlogs.com/api/clients/, use any name for app name and fill whatever website into URLs.
3. Set up your default directory in `config.json`
4. Run as `.\warcraftlogs-parser.exe COMBAT_ENCOUNTER` where `COMBAT_ENCOUNTER` can be either in form of URL `https://classic.warcraftlogs.com/reports/J1p4M8gd3b72RLGC` or `J1p4M8gd3b72RLGC` code directly