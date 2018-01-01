# Bot Framework FormFlow with date range validation

[![N|Solid](https://img.shields.io/badge/blog--at-wrapcode-red.svg?style=flat-square)](http://www.wrapcode.com/bot-framework-dependency-injection/?target=_blank)


A Bot Framework application that demonstrates different ways of **validating date ranges** in Bot Framework [FormFlow][df1] feature.

### Nuget Packages required (apart from minimum Bot Application Project)
  - [Microsoft.Recognizers.Text.DateTime][nuget-recognizers] (For Smart Date Recognition)

### Setup

 - Clone and open solution
 - Restore Packages
 - Start debugging
 - Open Bot Framework Emulator and enter this address - "http://localhost:3979/api/messages"
 - Start chatting
 
### Modules -
Project includes two date range validation approaches. Either of the approach is selected by Options Menu.
 1. **Simple Validatation** - With the help of validation function of `Field`, we can validate the date range as per business validations.
 
[![N|Solid](https://raw.githubusercontent.com/r4hulp/FormFlow-Date-Validations/master/BotFramework.FormFlow.Simple.thumb.png)](https://raw.githubusercontent.com/r4hulp/FormFlow-Date-Validations/master/BotFramework.FormFlow.Simple.png)

 2. **Smart Validation** - Microsoft's Recognizers library helps us to identify required date and time even if user writes a query in coloquial language e.g. *"First Monday of February 2018"* will return 5-Feb-2018 in DateTime format.
 
[![N|Solid](https://raw.githubusercontent.com/r4hulp/FormFlow-Date-Validations/master/BotFramework.FormFlow.Smart.thumb.png)](https://raw.githubusercontent.com/r4hulp/FormFlow-Date-Validations/master/BotFramework.FormFlow.Smart.png)

This sample also includes conditional flow for including / excluding weekends. This can be helpful in counting weekends.

### Development
Let's work together. Feel free to improve the code, request a pull.

### Contributors 
 - Rahul Patil


License
----
MIT

**Free free free!**

[df1]: <http://daringfireball.net/projects/markdown/>
[nuget-recognizers]: <https://www.nuget.org/packages/Microsoft.Recognizers.Text.DateTime/>
