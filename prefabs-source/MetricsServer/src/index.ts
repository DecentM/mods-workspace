import createServer from 'fastify'
import RateLimitPlugin from '@fastify/rate-limit'

import {log} from './log'
import {config} from './config'

import {register as ingest} from './api/metrics/ingest'
import {register as random} from './api/unsplash/random'

const main = async () => {
  const server = createServer({logger: log, trustProxy: config.api.trustProxy})

  if (config.api.rateLimit.enabled) {
    // Rate limit to 1 request per 5 seconds per IP, because the VRC video player has that rate limit as well,
    // so if someone is making requests faster, they have to be doing something weird
    await server.register(RateLimitPlugin, {
      max: config.api.rateLimit.max,
      timeWindow: config.api.rateLimit.window,
      allowList: config.api.rateLimit.allowList,
    })
  }

  await server.register(ingest, {prefix: 'api/v1'})
  await server.register(random, {prefix: 'api/v1'})

  await server.listen(process.env.PORT, process.env.HOST)
}

main().catch(log.error)
