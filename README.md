# Octopus

Dotnet component that works as a Hierarchical asynchronous task manager. There are situations that we might have to manage asynchronous tasks in Hierarchical fashion. Means, the parent task depends on child tasks to be completed. You can create a parent task, Set a custom asynchronous action delegate for it, create one or more child tasks and add to parent, you several options like you can execute them concurrently or in series. The overall progress will be updated to parent task. 
