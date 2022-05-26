import createServer from 'fastify'
import RateLimitPlugin from '@fastify/rate-limit'

import fs from 'node:fs'
import path from 'node:path'
import StatsD, {Tags} from 'hot-shots'
import {DateTime} from 'luxon'

import {config} from './config'
import {log} from './log'

type MetricsParamsWithValue = {
  name: string
  value?: string | null
}

const main = async () => {
  const statsd = new StatsD(config.statsd)

  const blankVideo = await fs.promises.readFile(path.join(__dirname, 'static/blank.mp4'))
  const server = createServer({logger: false})

  // Rate limit to 1 request per 5 seconds per IP, because the VRC video player has that rate limit as well,
  // so if someone is making requests faster, they have to be doing something weird
  await server.register(RateLimitPlugin, {
    max: 1,
    timeWindow: '5 seconds',
    allowList: ['127.0.0.1'],
  })

  server.head('/api/v1/metrics/ingest/:name/:value?', async (req, res) => {
    await res.status(200).send()
  })

  server.get<{Params: MetricsParamsWithValue}>('/api/v1/metrics/ingest/:name', async (req, res) => {
    await res.header('Content-Type', 'video/mp4').send(blankVideo)

    if (!req.headers['user-agent'].includes('NSPlayer')) return

    log.info(
      {
        name: req.params.name,
        query: req.query,
        ip: req.ip,
      },
      'metric received',
    )

    const tags: Tags = {
      ...(req.query as Tags),
      ip: req.ip,
      receivedAt: DateTime.now().toISO(),
    }

    statsd.increment(req.params.name, tags)
  })

  await server.listen(process.env.PORT)
}

main().catch(log.error)
