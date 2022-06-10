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
    trustProxy: boolean
    rateLimit: {
      enabled: boolean
      max: number
      window: string
      allowList: string[]
    }
  }

  unsplash: {
    accessKey: string
    secretKey: string
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
    trustProxy: process.env.API_TRUST_PROXY === 'true',
    rateLimit: {
      enabled: process.env.API_RATE_LIMIT_ENABLED === 'true',
      max: Number.parseInt(process.env.API_RATE_LIMIT_MAX, 10),
      window: process.env.API_RATE_LIMIT_WINDOW,
      allowList: process.env.API_RATE_LIMIT_ALLOWLIST.split(','),
    },
  },

  unsplash: {
    accessKey: process.env.UNSPLASH_ACCESS_KEY,
    secretKey: process.env.UNSPLASH_SECRET_KEY,
  },
}
