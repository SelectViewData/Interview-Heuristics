module InterviewTask.Tests

open InterviewTask.Api.Heuristics
open Xunit

let private crew crewId skills maxHours overtimeCap home =
  {
    crewId = crewId
    skills = skills |> List.map Skill |> Set.ofList
    maxHoursPerDay = maxHours
    overtimeCap = overtimeCap
    home = home
  }

let private job jobId requiredSkill hours location =
  {
    jobId = jobId
    requiredSkill = Skill requiredSkill
    estimatedHours = hours
    location = location
  }

[<Fact>]
let ``Score: travel uses Manhattan distance`` () =
  let crews = [ crew "C1" [ "General" ] 8 2 { x = 0; y = 0 } ]
  let assignments = [
    { crewId = "C1"
      jobs = [
        job "J1" "General" 1 { x = 3; y = 4 } // manhattan=7
      ] }
  ]

  let weights = { travelWeight = 2; overtimeWeight = 0; imbalanceWeight = 0 }
  let sc = score weights crews assignments

  Assert.Equal(14, sc.travelPenalty)
  Assert.Equal(0, sc.overtimePenalty)
  Assert.Equal(0, sc.imbalancePenalty)
  Assert.Equal(14, sc.total)

[<Fact>]
let ``Score: imbalance is L1 distance from avg hours`` () =
  let crews = [
    crew "C1" [ "General" ] 8 2 { x = 0; y = 0 }
    crew "C2" [ "General" ] 8 2 { x = 0; y = 0 }
  ]

  let assignments = [
    { crewId = "C1"; jobs = [ job "J1" "General" 6 { x = 0; y = 0 } ] }
    { crewId = "C2"; jobs = [ job "J2" "General" 2 { x = 0; y = 0 } ] }
  ]

  // avg = (6+2)/2 = 4; imbalance = |6-4| + |2-4| = 2+2 = 4
  let weights = { travelWeight = 0; overtimeWeight = 0; imbalanceWeight = 10 }
  let sc = score weights crews assignments

  Assert.Equal(40, sc.imbalancePenalty)
  Assert.Equal(40, sc.total)

[<Fact>]
let ``Greedy: unassigns job when no crew has the skill`` () =
  let crews = [ crew "C1" [ "General" ] 8 2 { x = 0; y = 0 } ]
  let jobs = [ job "J1" "Electrical" 2 { x = 0; y = 0 } ]
  let weights = { travelWeight = 1; overtimeWeight = 1; imbalanceWeight = 1 }

  let res = optimize weights 10 crews jobs

  Assert.Empty(res.assignments |> List.collect _.jobs)
  res.unassigned |> Assert.Single |> ignore

[<Fact>]
let ``Greedy: respects maxHours+overtimeCap (hard constraint)`` () =
  let crews = [ crew "C1" [ "General" ] 8 0 { x = 0; y = 0 } ]
  let jobs = [ job "J1" "General" 10 { x = 0; y = 0 } ]
  let weights = { travelWeight = 0; overtimeWeight = 100; imbalanceWeight = 0 }

  let res = optimize weights 10 crews jobs

  res.unassigned |> Assert.Single |> ignore
  Assert.Equal(0, res.score.overtimePenalty)

[<Fact>]
let ``Optimize: assignments always satisfy skill and capacity constraints`` () =
  let crews = [
    crew "C1" [ "General"; "Concrete" ] 8 2 { x = 0; y = 0 }
    crew "C2" [ "Electrical" ] 8 2 { x = 10; y = 0 }
  ]

  let jobs = [
    job "J1" "General" 6 { x = 1; y = 1 }
    job "J2" "Concrete" 4 { x = 2; y = 1 }
    job "J3" "Electrical" 8 { x = 10; y = 1 }
  ]

  let weights = { travelWeight = 1; overtimeWeight = 5; imbalanceWeight = 1 }
  let res = optimize weights 50 crews jobs

  let crewById = crews |> List.map (fun c -> c.crewId, c) |> Map.ofList

  for a in res.assignments do
    let crew = crewById[a.crewId]
    for j in a.jobs do
      Assert.True(crew.skills.Contains j.requiredSkill)

    let hours = a.jobs |> List.sumBy _.estimatedHours
    Assert.True(hours <= crew.maxHoursPerDay + crew.overtimeCap)

[<Fact>]
let ``Local search never makes the solution worse`` () =
  let crews = [
    crew "C1" [ "General"; "Concrete" ] 8 2 { x = 0; y = 0 }
    crew "C2" [ "General"; "Concrete" ] 8 2 { x = 10; y = 0 }
  ]

  let jobs = [
    job "J1" "General" 6 { x = 0; y = 0 }
    job "J2" "Concrete" 6 { x = 10; y = 0 }
    job "J3" "General" 2 { x = 0; y = 0 }
  ]

  let weights = { travelWeight = 1; overtimeWeight = 10; imbalanceWeight = 1 }

  let init = initialGreedy weights crews jobs
  let improved = improveLocalSearch weights 50 crews init

  Assert.True(improved.score.total <= init.score.total)