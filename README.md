dERP.Inventory
===============
This is an expansion on my earlier spike of Nancy.Commandly. 

This time I used the actual domain of Inventory (of course it is just a fraction of the real domain, but it IS a sample!). I chose this domain as I used to work at an import / export operation. We had our own warehouse. We also had an ERP system to manage all of it.

If you've seen any ERP system, you probably noticed that each menu item / context has a LOT of information associated with it. For example a typical inventory screen will contain many tabs like how many on hand, the locations / bins they are in, costing information, how it is sold, how it is taxed, what account it posts to.... 

ARGH! Most of that stuff has nothing to do with Inventory. The stock boy really only cares about where it is so he can put it away / pick it. His manager MAY care about dimensions and movement if he wants to optimize locations for picking / receiving (we certainly didn't). No one working in the warehouse cares about LIFO / FIFO / Average costing, tax-ability, posting groups, etc. All those other concerns probably belong to accounting.

dERP.Inventory.Web
===============
This is where all the non-domainy stuff lives.

Commands
Most of the time you are just using straight primitives in your command model. There's a nice convention where you just call `Register[of TCommand]` in the bootstrapper. The sample will register the handler on the bus and register a NancyModule that maps to it. Simply place your view in `views/dispatcher` and it's done.

If you need to do something that is not part of your command model, you can use a different module. E.g. your command triggers the processing of large blobs, but putting large byte arrays on your command is probably not a good idea. Instead you want to upload the assets to S3 and put the links to them on the command. In this case call `Register[of TCommand, TModule]` instead.

Read Models
One of the advantages with ES + CQRS is the disposability of the read model. It can also be a huge hassle - I've needed to take systems down to rebuild a read model as the rebuild code lived in separate utility app. Also, you probably want to take advantage of your read model store's batching capabilities when doing this.

With EventStore this problem can go away as it has a notion of global Position. Store the position transactionally with your read model. If you find a bug, drop the read model and set its Position back to Position.Start. The catch up code will know what to do. Also, it will know when you have more or less caught up. Then you can switch from batch mode to immediate mode.

The other advantage is that you can select the right read model store for that particular read model. The sample started by using an in memory store for everything, then switched to ravendb for item search.

How do I run it?
===============
You'll have to get http://www.geteventstore.com and http://www.ravendb.net/. The sample assumes both are on localhost and their default ports - 1113 and 8080 respectively. 

Run the Importer project. This grabs all the products from http://householdproducts.nlm.nih.gov and turns them into WarehouseItems, our aggregate root. The .gov site is a little flaky, you may have to run it a couple of times to make it work.

Lastly run `dERP.Inventory.Web.SelfHost.`

???

Profit!!
