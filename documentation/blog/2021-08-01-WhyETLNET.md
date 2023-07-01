---
title: Why ETL.NET?
author: Stéphane Royer
author_title: ETL.NET Lead developer
author_url: https://github.com/paillave
author_image_url: https://avatars.githubusercontent.com/paillave
---

# Why ETL.NET?

Years after years, contexts I faced led me to the conclusion that ETL engines (even SSIS that is that is still one of the bests 15 years after its initial release) should now reach a next generation.

<!--truncate-->

:astonished: SSIS for instance, misses so many out of the box features that make *a lot* of developers very frustrated. Just to mention one typical example among many I could enumerate: the terrible absence of an efficient and fast upsert.

:confused: As most of ETL engines are nearly exclusively focused on performance, I noticed that in real life, they are mostly used to integrate relatively small volumes of data. This reveals a wrong alignment between offer and demand. Most developers need to process between thousands and hundreds thousands rows, but editors keep on advertising how good they are in the integration of billions rows! At the end of the day, the sad fact is... indeed, ETL engines are monsters of performance when it is about to import billions of records... but for real life usual use cases, it is too heavy and very unproductive to develop with it. Not even to mention their integration in an application architecture.

:smirk: Here, many BI specialists would answer the following:

> BI is a discipline of IT that is different, with a specific architecture that must be followed to get the best results. Ralph Kimball or Bill Inmon approaches shall be studied. Your vision is too much biased by your development approach.

:pensive: My personal belief is that the world changed; now it is this vision that is biased by an old fashioned BI approach. Computers are, like always, way more powerful than before: extracting, transforming and loading hundreds thousands rows of data is now a problem for nearly nobody anymore. Nowadays, the main problem is this one:

> Can we implement **with an acceptable effort** an efficient, maintainable and complex ETL process in any simple architecture?

:bulb: ETL.NET Is meant to solve this problem.
For BI developers, it will look like... a simple .NET library. Nothing to do with heavy weighed tools with visual designers!

ETL.NET has been done in the context of a [financial solution](https://www.fundprocess.lu) and it would be fantastic to see it being used by a larger community than myself and the team of [FundProcess](https://www.fundprocess.lu).
