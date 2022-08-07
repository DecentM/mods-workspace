import {FastifyPluginAsync, FastifyPluginOptions} from 'fastify'

import fs from 'node:fs'
import path from 'node:path'

import {DateTime} from 'luxon'
import hasha from 'hasha'

import {config} from '../../config'
import {collect} from './collect'

export type MetricsParamsWithValue = {
  name: string
  value?: string | null
}

export const register: FastifyPluginAsync<FastifyPluginOptions> = async (instance) => {
  const videoPath = path.join(__dirname, '../..', 'static/blank.mp4')
  const blankVideo = await fs.promises.readFile(videoPath)

  instance.head('/metrics/ingest/:name', async (req, res) => {
    await res.status(200).send()
  })

  instance.get<{Params: MetricsParamsWithValue}>('/metrics/ingest/:name', async (req, res) => {
    const promises = []

    if (req.headers['user-agent'].includes('NSPlayer')) {
      const tags: Record<string, string> = {
        ...(req.query as Record<string, string>),
        ipHash: config.privacy.ip === 'hash' ? await hasha.async(req.ip) : null,
        ip: config.privacy.ip === 'include' ? req.ip : null,
        receivedAt: DateTime.now().toISO(),
      }

      instance.log.debug(
        {
          name: req.params.name,
          tags,
        },
        'metric received',
      )

      promises.push(collect(req.ip, req.params.name, tags))
    }

    promises.push(res.header('Content-Type', 'video/mp4').send(blankVideo))

    await Promise.all(promises)
  })

  await Promise.resolve()
}
