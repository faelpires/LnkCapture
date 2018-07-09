# LnkCapture
LnkCapture Bot for Telegram - https://t.me/LnkCaptureBot

The bot automatically saves the links you send to a chat and allows you to retrieve them later in a web interface. It also provides REST query for links to be retrieved on third-party systems.

How to consume the REST API:

JavaScript (jQuery)
```javascript
$.ajax({
   url: "https://lnkcapture.com/api/link/f5021f2f-806c-4f03-8b56-5a76d095b5de",
   type: "GET",
   data: {
       search: null,
       user: null,
       startDate: '2018-07-01',
       endDate: '2020-07-01',
       pageIndex: 0,
       pageSize: 10
   },
   contentType: "application/json",
   success: function(data) {
       console.log(data);
   },
   error: function(args){
       console.log(args);
   }
});
```
