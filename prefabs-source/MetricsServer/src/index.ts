import createServer from 'fastify'
import createLogger from 'pino'
import fs from 'node:fs'
import path from 'node:path'

const log = createLogger()

type MetricsParamsWithValue = {
  name: string
  value?: string | null
}

const main = async () => {
  const blankVideo = await fs.promises.readFile(path.join(__dirname, 'static/blank.mp4'))
  const server = createServer({logger: false})

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
      },
      'metric received',
    )
  })

  await server.listen(process.env.PORT)
}

main().catch(log.error)
