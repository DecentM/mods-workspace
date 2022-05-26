import {ClientOptions} from 'hot-shots'
import {log} from './log'

type Config = {
  statsd: ClientOptions & {
    enabled: boolean
  }

  ga: {
    enabled: boolean
    tid: string
    collectUrl: string
    ua: string
  }

  api: {
    rateLimit: {
      enabled: boolean
      max: number
      window: string
    }
  }
}

export const config: Config = {
  statsd: {
    enabled: process.env.STATSD_ENABLED === 'true',

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

  ga: {
    enabled: process.env.GOOGLE_ANALYTICS_ENABLED === 'true',
    tid: process.env.GOOGLE_ANALYTICS_PROPERTY_ID,
    collectUrl: process.env.GOOGLE_ANALYTICS_COLLECT_URL,
    ua: process.env.GOOGLE_ANALYTICS_UA,
  },

  api: {
    rateLimit: {
      enabled: process.env.API_RATE_LIMIT_ENABLED === 'true',
      max: Number.parseInt(process.env.API_RATE_LIMIT_MAX, 10),
      window: process.env.API_RATE_LIMIT_WINDOW,
    },
  },
}
