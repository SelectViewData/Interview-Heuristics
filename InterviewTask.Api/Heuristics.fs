module InterviewTask.Api.Heuristics

// -------------------------------
// Domain
// -------------------------------

type Skill = Skill of string

type Location = { x: int; y: int }

type Job =
  { jobId: string
    requiredSkill: Skill
    estimatedHours: int
    location: Location }

type Crew =
  { crewId: string
    skills: Set<Skill>
    maxHoursPerDay: int
    overtimeCap: int
    home: Location }

type Weights =
  { travelWeight: int
    overtimeWeight: int
    imbalanceWeight: int }

type Assignment =
  { crewId: string
    jobs: Job list }

type ScoreBreakdown =
  { travelPenalty: int
    overtimePenalty: int
    imbalancePenalty: int
    total: int }

type Solution =
  { assignments: Assignment list
    unassigned: Job list
    score: ScoreBreakdown }

// -------------------------------
// Utilities
// -------------------------------

let private manhattan (a: Location) (b: Location) =
  abs (a.x - b.x) + abs (a.y - b.y)

let private hasSkill (crew: Crew) (job: Job) =
  crew.skills.Contains job.requiredSkill

let private totalHours (jobs: Job list) =
  jobs |> List.sumBy (fun j -> j.estimatedHours)

let private withinCap (crew: Crew) (jobs: Job list) =
  let h = totalHours jobs
  h <= crew.maxHoursPerDay + crew.overtimeCap

let private isFeasible (crews: Crew list) (assignments: Assignment list) =
  let crewById =
    crews |> List.map (fun c -> c.crewId, c) |> Map.ofList

  assignments
  |> List.forall (fun a ->
    let crew = crewById[a.crewId]
    withinCap crew a.jobs)

// -------------------------------
// Scoring
// -------------------------------

let score (weights: Weights) (crews: Crew list) (assignments: Assignment list) : ScoreBreakdown =
  let crewById =
    crews |> List.map (fun c -> c.crewId, c) |> Map.ofList

  let hoursPerCrew =
    assignments
    |> List.map (fun a -> a.crewId, totalHours a.jobs)

  let avg =
    if hoursPerCrew.IsEmpty then 0
    else (hoursPerCrew |> List.sumBy snd) / hoursPerCrew.Length

  let perCrew =
    assignments
    |> List.map (fun a ->
      let crew = crewById[a.crewId]
      let assignedHours = totalHours a.jobs

      if not (withinCap crew a.jobs) then
        failwith $"Crew {crew.crewId} exceeds maxHoursPerDay + overtimeCap"

      let travelDistance =
        a.jobs |> List.sumBy (fun j -> manhattan crew.home j.location)

      let overtimeHours = max 0 (assignedHours - crew.maxHoursPerDay - 1)

      let imbalanceHours = abs (assignedHours - avg)

      let travelPenalty = travelDistance * weights.travelWeight
      let overtimePenalty = overtimeHours * weights.overtimeWeight
      let imbalancePenalty = imbalanceHours * weights.imbalanceWeight

      travelPenalty, overtimePenalty, imbalancePenalty)

  let travelPenalty = perCrew |> List.sumBy (fun (t, _, _) -> t)
  let overtimePenalty = perCrew |> List.sumBy (fun (_, o, _) -> o)
  let imbalancePenalty = perCrew |> List.sumBy (fun (_, _, i) -> i)
  let total = travelPenalty + overtimePenalty + imbalancePenalty

  { travelPenalty = travelPenalty
    overtimePenalty = overtimePenalty
    imbalancePenalty = imbalancePenalty
    total = total }

// -------------------------------
// Construct initial (greedy) solution
// -------------------------------

let initialGreedy (weights: Weights) (crews: Crew list) (jobs: Job list) : Solution =
  let emptyAssignments =
    crews |> List.map (fun c -> { crewId = c.crewId; jobs = [] })

  // Sort "hard" jobs first: longer + rarer skill
  let skillCounts =
    jobs
    |> List.groupBy (fun j -> j.requiredSkill)
    |> List.map (fun (s, js) -> s, js.Length)
    |> Map.ofList

  let hardness (j: Job) =
    let rarity = skillCounts[j.requiredSkill] // smaller -> rarer
    // prefer rarer and longer first
    (rarity, -j.estimatedHours)

  let sortedJobs = jobs |> List.sortBy hardness

  let addJobToCrew (crewId: string) (job: Job) (assignments: Assignment list) =
    assignments
    |> List.map (fun a ->
      if a.crewId = crewId then { a with jobs = job :: a.jobs } else a)

  // Choose best feasible crew by minimal incremental score increase
  let rec loop (assignments: Assignment list) (unassigned: Job list) (remaining: Job list) =
    match remaining with
    | [] ->
        let normalized =
          assignments |> List.map (fun a -> { a with jobs = List.rev a.jobs })
        let sc = score weights crews normalized
        { assignments = normalized; unassigned = List.rev unassigned; score = sc }

    | job :: rest ->
        let feasibleCrews =
          crews
          |> List.filter (fun c -> hasSkill c job)
          |> List.filter (fun c ->
            let current = assignments |> List.find (fun a -> a.crewId = c.crewId)
            withinCap c (job :: current.jobs))

        match feasibleCrews with
        | [] ->
            loop assignments (job :: unassigned) rest
        | _ ->
            let currentScore = score weights crews assignments

            let bestCrew =
              feasibleCrews
              |> List.minBy (fun c ->
                let trial = addJobToCrew c.crewId job assignments
                let trialScore = score weights crews trial
                trialScore.total - currentScore.total)

            let updated = addJobToCrew bestCrew.crewId job assignments
            loop updated unassigned rest

  loop emptyAssignments [] sortedJobs

// -------------------------------
// Local search improvement
// -------------------------------

type Move =
  | MoveJob of jobId: string * fromCrewId: string * toCrewId: string
  | SwapJobs of jobIdA: string * crewA: string * jobIdB: string * crewB: string

let private removeJob (jobId: string) (jobs: Job list) =
  jobs |> List.filter (fun j -> j.jobId <> jobId)

let private tryFindJob (jobId: string) (jobs: Job list) =
  jobs |> List.tryFind (fun j -> j.jobId = jobId)

let private applyMove (crews: Crew list) (move: Move) (assignments: Assignment list) : Assignment list option =
  let crewById = crews |> List.map (fun c -> c.crewId, c) |> Map.ofList

  let updateCrewJobs (crewId: string) (f: Job list -> Job list) (assignments: Assignment list) =
    assignments
    |> List.map (fun (a: Assignment) -> if a.crewId = crewId then { a with jobs = f a.jobs } else a)

  match move with
  | MoveJob (jobId, fromId, toId) ->
      let fromA = assignments |> List.find (fun a -> a.crewId = fromId)
      match tryFindJob jobId fromA.jobs with
      | None -> None
      | Some job ->
          if not (hasSkill crewById[toId] job) then None
          else
            let updated =
              assignments
              |> updateCrewJobs fromId (removeJob jobId)
              |> updateCrewJobs toId (fun jobs -> job :: jobs)

            if isFeasible crews updated then Some updated else None

  | SwapJobs (jobIdA, crewA, jobIdB, crewB) ->
      let aA = assignments |> List.find (fun a -> a.crewId = crewA)
      let aB = assignments |> List.find (fun a -> a.crewId = crewB)

      match tryFindJob jobIdA aA.jobs, tryFindJob jobIdB aB.jobs with
      | Some jobA, Some jobB ->
          if not (hasSkill crewById[crewA] jobB) then None
          elif not (hasSkill crewById[crewB] jobA) then None
          else
            let afterRemove =
              assignments
              |> List.map (fun (a: Assignment) ->
                if a.crewId = crewA then { a with jobs = removeJob jobIdA a.jobs }
                elif a.crewId = crewB then { a with jobs = removeJob jobIdB a.jobs }
                else a)

            let afterAdd =
              afterRemove
              |> List.map (fun (a: Assignment) ->
                if a.crewId = crewA then { a with jobs = jobB :: a.jobs }
                elif a.crewId = crewB then { a with jobs = jobA :: a.jobs }
                else a)

            if isFeasible crews afterAdd then Some afterAdd else None
      | _ -> None

let private allMoves (assignments: Assignment list) : Move list =
  let crewIds = assignments |> List.map (fun a -> a.crewId)

  let jobsByCrew =
    assignments
    |> List.collect (fun a -> a.jobs |> List.map (fun j -> a.crewId, j))

  let moveMoves =
    jobsByCrew
    |> List.collect (fun (fromId, job) ->
      crewIds
      |> List.filter (fun toId -> toId <> fromId)
      |> List.map (fun toId -> MoveJob (job.jobId, fromId, toId)))

  let takeN = 10

  let perCrewJobs =
    assignments
    |> List.map (fun a -> a.crewId, (a.jobs |> List.truncate takeN))
    |> Map.ofList

  let swapMoves =
    [ for i in 0 .. crewIds.Length - 1 do
        for k in i + 1 .. crewIds.Length - 1 do
          let cA = crewIds[i]
          let cB = crewIds[k]
          for jA in perCrewJobs[cA] do
            for jB in perCrewJobs[cB] do
              yield SwapJobs (jA.jobId, cA, jB.jobId, cB) ]

  moveMoves @ swapMoves

let improveLocalSearch (weights: Weights) (maxIters: int) (crews: Crew list) (solution: Solution) : Solution =
  let rec loop iters current =
    if iters <= 0 then current
    else
      let currentScore = score weights crews current.assignments
      let moves = allMoves current.assignments

      let bestCandidate =
        moves
        |> List.choose (fun m ->
          applyMove crews m current.assignments
          |> Option.map (fun a -> a, score weights crews a))
        |> List.sortBy (fun (_, sc) -> sc.total)
        |> List.tryHead

      match bestCandidate with
      | None ->
          { current with score = currentScore }
      | Some (bestAssignments, bestScore) ->
          if bestScore.total < currentScore.total then
            loop (iters - 1) { current with assignments = bestAssignments; score = bestScore }
          else
            { current with score = currentScore }

  loop maxIters solution

// -------------------------------
// Main entry: optimize
// -------------------------------

let optimize (weights: Weights) (maxIters: int) (crews: Crew list) (jobs: Job list) : Solution =
  let init = initialGreedy weights crews jobs
  improveLocalSearch weights maxIters crews init

