import {FastifyPluginAsync, FastifyPluginOptions} from 'fastify'

import fs from 'node:fs'
import path from 'node:path'

import {DateTime} from 'luxon'
import {collect} from './collect'

export type MetricsParamsWithValue = {
  name: string
  value?: string | null
}

export const register: FastifyPluginAsync<FastifyPluginOptions> = async (instance) => {
  instance.head('/metrics/ingest/:name', async (req, res) => {
    await res.status(200).send()
  })

  instance.get<{Params: MetricsParamsWithValue}>('/metrics/ingest/:name', async (req, res) => {
    const blankVideo = await fs.promises.readFile(path.join(__dirname, '../..', 'static/blank.mp4'))
    await res.header('Content-Type', 'video/mp4').send(blankVideo)

    if (!req.headers['user-agent'].includes('NSPlayer')) return

    instance.log.info(
      {
        name: req.params.name,
        query: req.query,
        ip: req.ip,
      },
      'metric received',
    )

    const tags: Record<string, string> = {
      ...(req.query as Record<string, string>),
      ip: req.ip,
      receivedAt: DateTime.now().toISO(),
    }

    await collect(req.ip, req.params.name, tags)
  })

  await Promise.resolve()
}
