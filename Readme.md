# A Victorian Take on Rich Domain Plumbing

This code was written for a talk with the title above.  The aim was to show the difference between code written with Anaemic domains and code written with a Rich domain

Slides are available with notes in the folder

Code was written completely TDD although was written without mocks and an in memory database as this is how we write tests at Victorian Plumbing.  This was to demonstrate that unit tests are not sufficient for protecting us from problems in our code

Whilst unit tests are very important, they are only as good as the knowledge of the developer who wrote them

The starting point is to check the code at this commit - https://github.com/davemanton/RichDomains/commit/d232f0a2f02f2959c06da0cda11d38b3960a4a33 named "*** Finished anaemic domain example ***"

At this stage the the code has been written with an anaemic domain model.  Whilst the OrderCreator is okay and looks just like a standard class, the OrderUpdater has been designed to show how it can be done worryingly wrong and there was nothing there to guide the developer to stop it other than what they knew

The next stage is to check the code at this commit - https://github.com/davemanton/RichDomains/commit/c7f28eebedfd23bda6e84774dadf8fa071ec470e named "Moved to basic rich domain model"

At this stage the code has demonstrated how a very simple change to constructors, private setters/init and private collections combined with basic domain behaviors can start to protect our domain from some of the mistakes in the OrderUpdater

The next stage is to check the code at this commit - https://github.com/davemanton/RichDomains/commit/afae30eb5c23d7af408586b0838cbafbbedb1d43 named "*** Final version of more advanced domain model prior to update ***"

This shows how additional methods such as using a create method on the order can help to bring validation into the domain with the use of domain services and the visitor pattern (it may not be to everyone's tastes)

Finally the order updater is rewritten at this commit - https://github.com/davemanton/RichDomains/commit/d2e4a0bf6f6a1a8d7546096efb3d7b3f846b741f named "*** Completed Order Updater ***"

This shows how the situation in the original order updater could be avoided with the help of the domain as the encapsulated rich behaviours force the developer to decompose the problem

