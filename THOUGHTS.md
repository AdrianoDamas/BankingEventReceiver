# What things did you considered of during the implementation?

So here are the following key assumption, considerations, and decisions I have made during the development.

## Overall architecture
Since this is a banking application, clean architecture is crucial to ensure secure financial operations. That is why I eventually picked a bit of DDD approach (at least in the domain entities design) and also followed Clean Architecture in terms of organization and dependencies flow. Hence we can clearly see the following layers:
- Application - contains orchestration logic and repositories interfaces.
- Domain - contains core entities, exceptions, and enums.
- Infrastructure - contains DB related entities, repository implementations, etc.
- Common - contains cross-cutting concerns (in this case messaging generic processing part)
- Migrations left in the root just for simplicity. If I created the project(s) within each layer, then Migrations would be inside the Data-related project.

I intentionally haven't created any additional projects for simplicity. But still there is a clear dependency flow according to Clean Architecture principles.

Of course everything can be implemented in a bit or much simpler way. But just considering the domain I decided to go with this approach to make it clean and extensible.

## Infrastructure Layer
- I didn't touch the migrations at all. So normally we need to generate new ones based on the entity configurations
- I added entity configurations for the existing table, as well as for the new one I have implemented.
- Transactions are needed there to ensure idempotency. Also we can implement the background service that would do nightly reconciliation job and re-calculate balances based on the actual transactions from the previous day (that is why there is TransactionType.Reconciliation)
- I decided to go with repositories pattern for better separation of concerns. For the same reason I decided to fetch the entity before changing it. Yes this will affect the performance a bit, but this is anyways background worker.
- I follow optimistic concurrency principle here, so I added row version to the BankAccounts table.
Anything was unclear? (BTW both the domain model and repository support several transactions processing at a time, but this feature is never used from the App layer at the moment, so we might want to simplify based on our future plans).

## Domain Layer
Nothing specific here. Just kind-of reach models with embedded validation, several enums, and domain exceptions. Account is considered aggregate root here.

## Common Layer
I moved generic message processing and abstractions here, since the application layer has nothing to do with the message parsing and service bus itself.

## Application Layer
- Implements parser for the message (used by the generic MessageProcessor<>)
- Defines and implements transaction processor
- Defines repository interface for Account
- Defines and implements IBankingApplication - the main orchestrator here (yes, I didn't like the default MessageWorker name)

## Host level
I have added BankingBackgroundService that derives from BackgroundService, and just calls the Banking application.

# Testing
Well, I think everything here of course should be covered with unit-tests. And integration tests. And E2E tests. Due to the lack of my free time now I am covering only the domain and application layers (but still not everything in there, for instance I skipped BankingApplication tests, because I just don't have time now)

# What can be done additionally?
Actually a lot of things. But since the time is limited here, and our budget is unclear, I assumed we have enough time to implement things properly, but still skipped a bit of implementations here and there because I don't have much time now.
We can (besides everything else) extract retry configuration into options.

# Disclaimer
I might have missed something. Everything should be properly tested and reviewed. And covered with some e2e automation.
