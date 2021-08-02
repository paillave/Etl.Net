---
sidebar_position: 1
---

# How it works

![How it works](/img/microservices-tutorial-bot-laptop-table.svg)

The ETL.NET principle is to have a set of operators that emits events based on events they received from other operators. They report their activity to the runtime that controls them. All this forming a cascade of events. This is a.... ***stream*** !

## Detailed principle

- The purpose of an ETL.NET process is to create `nodes` that we call `operators`.
- Each operator listens one or several source of `events`.
- Depending on source events, an operator will emit an output event. What is emitted depends on the purpose of the operator.
- On top of broadcasting its events to all its listeners, every operator will report this event to the runtime. Events can be of two types. The first type, `content event`, contains the payload, the actual value that is the result of the operation. The other one, `end of stream event`, notifies that no content event will.
- When an error occurs in the operator, this one emits it to the runtime.
- When the runtime is notified of an error, it requests every operator to stop its job, then stops, and then returns a failed execution status along with the notified error.
- When the runtime received the `end of stream event` from every operator of the process, it stops, and then returns a successful execution status.
- The runtime triggers the process by raising a single event containing parameters of the ETL process into the `trigger stream`. This is this trigger stream that is given as a parameter to the function that builds the ETL process. This method is generally called `DefineProcess` by convention.
- The point of the `DefineProcess` method is to setup operators for them to listen other operators, or to listen the trigger stream.
- Every operator notification or raised error that is reported to the runtime is "poured" into the trace stream that will be processed accordingly to what is defined in the definition of the trace process (if any). The method that defines the trace process is called `DefineTraceProcess` by convention.

## An operator illustrated

![Operators](/img/operators.svg)

Above, the operator.

- It listens one or several source of `events`
- It emits output events
- It reports events to the runtime
- It reports error to the runtime

## The stream of events, a global picture

The following illustration pictures how operators interact with each other, making a stream of events.

![Streams](/img/operators-in-stream.svg)

## More?

To quickly see how to get your hands on ETL.NET, go to [install and run ETL.NET](/docs/quickstart/installation).