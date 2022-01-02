from gym.envs.registration import register

register(
    id='patrolling-v0',
    entry_point='PatrollingProblem.envs:PatrollingProblemEnv',
)