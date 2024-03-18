# VfxApi

Hello, welcome to my Currency Exchange API.

In this API you can Post a new Currency Exchange, Delete a Currency Exchange, get all the Currencies Exchanges or just get one. WHen getting one if it does note exists in the database ( I'm using a potsgres database) it goes to an external API to check if that Currency Exchange exists there. If it exists in the external API it saves in the database of my API for future checks, if not it gives a message saying that the desired currency exchange was not found the in external API.

I commented the Controller to explain how the API works.
