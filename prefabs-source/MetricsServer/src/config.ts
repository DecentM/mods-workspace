import {ClientOptions} from 'hot-shots'
import {log} from './log'

type Config = {
  statsd: ClientOptions
}

export const config: Config = {
  statsd: {
    host: process.env.STATSD_HOST,
    port: Number.parseInt(process.env.STATSD_PORT, 10),
    sampleRate: Number.parseFloat(process.env.STATSD_SAMPLE_RATE),
    globalTags: {
      service: 'com.decentm.vrchat.metricsserver',
    },
    errorHandler(err) {
      log.error(err, 'cannot send to statsd')
    },
  },
}
